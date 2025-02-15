"""empty message

Revision ID: bb0bab94a39a
Revises: 9928384a3a0d
Create Date: 2024-10-30 10:14:54.756915

"""
from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision = 'bb0bab94a39a'
down_revision = '9928384a3a0d'
branch_labels = None
depends_on = None


def upgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.create_table('multiple_choice_challenge',
    sa.Column('id', sa.Integer(), nullable=False),
    sa.ForeignKeyConstraint(['id'], ['challenges.id'], ondelete='CASCADE'),
    sa.PrimaryKeyConstraint('id')
    )
    # ### end Alembic commands ###


def downgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.drop_table('multiple_choice_challenge')
    # ### end Alembic commands ###
